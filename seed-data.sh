#!/usr/bin/env bash
# Seeds demo data into the running Explorer SOA stack through the API gateway.
# Idempotency: re-running creates duplicate tours/blogs but won't break anything.
set -uo pipefail

G="${GATEWAY:-http://localhost:8080}"
JQ=jq

say() { printf "\n\033[1m%s\033[0m\n" "$*"; }
ok()  { printf "  ✓ %s\n" "$*"; }

# --- auth helpers ---------------------------------------------------------
register() { # username password name surname email role  -> prints "id token"
  curl -s -X POST "$G/api/users" -H 'Content-Type: application/json' \
    -d "{\"username\":\"$1\",\"password\":\"$2\",\"name\":\"$3\",\"surname\":\"$4\",\"email\":\"$5\",\"role\":$6}" \
    | $JQ -r '"\(.id) \(.accessToken)"' 2>/dev/null
}
login() { # username password -> prints "id token"
  curl -s -X POST "$G/api/users/login" -H 'Content-Type: application/json' \
    -d "{\"username\":\"$1\",\"password\":\"$2\"}" \
    | $JQ -r '"\(.id) \(.accessToken)"' 2>/dev/null
}
ensure_user() { # username password name surname email role -> prints "id token" (login or register)
  local out
  out=$(login "$1" "$2")
  if [[ "$out" == "null null" || -z "$out" ]]; then
    out=$(register "$1" "$2" "$3" "$4" "$5" "$6")
  fi
  echo "$out"
}

# --- domain helpers -------------------------------------------------------
create_published_tour() { # token authorId name desc difficult price tags  -> prints tourId
  local tok=$1 author=$2 name=$3 desc=$4 diff=$5 price=$6 tags=$7
  local tour tourId
  tour=$(curl -s -X POST "$G/api/tour" -H "Authorization: Bearer $tok" -H 'Content-Type: application/json' \
    -d "{\"authorId\":$author,\"name\":\"$name\",\"description\":\"$desc\",\"difficult\":$diff,\"status\":0,\"price\":0,\"tags\":\"$tags\",\"distance\":0}")
  tourId=$(echo "$tour" | $JQ -r '.id')
  [ "$tourId" = "null" ] && { echo "0"; return; }

  # set price + a travel time (WALKING=2), keeping it a draft for now
  curl -s -X PUT "$G/api/tour/updatetour" -H "Authorization: Bearer $tok" -H 'Content-Type: application/json' \
    -d "{\"id\":$tourId,\"authorId\":$author,\"name\":\"$name\",\"description\":\"$desc\",\"difficult\":$diff,\"status\":0,\"price\":$price,\"tags\":\"$tags\",\"distance\":3.2,\"travelTimeAndMethod\":[{\"travelTime\":90,\"travelMethod\":2}],\"tourEquipment\":[]}" >/dev/null

  # two key points (publish requires >= 2); PublicRequest is required by validation
  curl -s -X POST "$G/api/tour/addCheckpoint" -H "Authorization: Bearer $tok" -H 'Content-Type: application/json' \
    -d "{\"name\":\"Polazna tacka\",\"description\":\"Pocetak ture\",\"pictureURL\":\"start.jpg\",\"latitude\":44.8176,\"longitude\":20.4569,\"tourId\":$tourId,\"publicRequest\":{\"status\":0,\"comment\":\"\"}}" >/dev/null
  curl -s -X POST "$G/api/tour/addCheckpoint" -H "Authorization: Bearer $tok" -H 'Content-Type: application/json' \
    -d "{\"name\":\"Krajnja tacka\",\"description\":\"Kraj ture\",\"pictureURL\":\"end.jpg\",\"latitude\":44.8225,\"longitude\":20.4612,\"tourId\":$tourId,\"publicRequest\":{\"status\":0,\"comment\":\"\"}}" >/dev/null

  # publish
  curl -s -X PUT "$G/api/tour/publishTour" -H "Authorization: Bearer $tok" -H 'Content-Type: application/json' \
    -d "$tourId" >/dev/null
  echo "$tourId"
}

create_blog() { # token authorId name desc  -> prints blogId
  curl -s -X POST "$G/api/blogs/" -H "Authorization: Bearer $1" -H 'Content-Type: application/json' \
    -d "{\"name\":\"$3\",\"description\":\"$4\",\"dateCreated\":\"2026-06-10T10:00:00Z\",\"images\":[],\"authorId\":$2,\"author\":\"x\",\"status\":1,\"rating\":0,\"comments\":[],\"ratings\":[]}" \
    | $JQ -r '.id'
}

follow() { curl -s -X POST "$G/api/followers/users" -H 'Content-Type: application/json' -d "{\"followerId\":$1,\"followedId\":$2}" >/dev/null; }

buy_tour() { # token userId tourId tourName price
  curl -s -X PUT "$G/api/shoppingCart/addTour/$2" -H "Authorization: Bearer $1" -H 'Content-Type: application/json' \
    -d "{\"tourId\":$3,\"tourName\":\"$4\",\"price\":$5}" >/dev/null
  curl -s -X POST "$G/api/order/$2" -H "Authorization: Bearer $1" -H 'Content-Type: application/json' -d "[$3]" >/dev/null
}

# =========================================================================
say "1) Korisnici"
read -r AUTOR_ID AUTOR_TOK   < <(login autor autor)
read -r TURISTA_ID TURISTA_TOK < <(login turista turista)
ok "autor=$AUTOR_ID, turista=$TURISTA_ID (seed)"

read -r ANA_ID ANA_TOK       < <(ensure_user ana ana123 Ana Anic ana@explorer.com 1)
read -r NIKOLA_ID NIKOLA_TOK < <(ensure_user nikola nikola123 Nikola Nikolic nikola@explorer.com 1)
read -r JOVANA_ID JOVANA_TOK < <(ensure_user jovana jovana123 Jovana Jovic jovana@explorer.com 2)
read -r STEFAN_ID STEFAN_TOK < <(ensure_user stefan stefan123 Stefan Stefanovic stefan@explorer.com 2)
ok "vodici: ana=$ANA_ID, nikola=$NIKOLA_ID"
ok "turisti: jovana=$JOVANA_ID, stefan=$STEFAN_ID"

say "2) Ture (objavljene, sa 2 kljucne tacke + travel time + cena)"
T1=$(create_published_tour "$AUTOR_TOK"  "$AUTOR_ID"  "Beogradska tvrdjava"   "Setnja kroz Kalemegdan i Stari grad" 0 1200 "istorija,setnja,grad")
ok "T1 (autor): Beogradska tvrdjava = $T1"
T2=$(create_published_tour "$AUTOR_TOK"  "$AUTOR_ID"  "Skadarlija bohem tura" "Boemska cetvrt i restorani"          1 900  "hrana,nocni,grad")
ok "T2 (autor): Skadarlija = $T2"
T3=$(create_published_tour "$ANA_TOK"    "$ANA_ID"    "Novi Sad i Petrovaradin" "Tvrdjava i Dunavski park"          1 1500 "priroda,istorija")
ok "T3 (ana): Novi Sad = $T3"
# jedna draft tura
DRAFT=$(curl -s -X POST "$G/api/tour" -H "Authorization: Bearer $NIKOLA_TOK" -H 'Content-Type: application/json' \
  -d "{\"authorId\":$NIKOLA_ID,\"name\":\"Tara nacionalni park (draft)\",\"description\":\"U pripremi\",\"difficult\":2,\"status\":0,\"price\":0,\"tags\":\"priroda,planinarenje\",\"distance\":0}" | $JQ -r '.id')
ok "DRAFT (nikola): Tara = $DRAFT"

say "3) Blogovi"
B1=$(create_blog "$AUTOR_TOK" "$AUTOR_ID" "Najlepsi vidikovci Beograda" "Moja lista omiljenih vidikovaca u gradu.")
B2=$(create_blog "$ANA_TOK"   "$ANA_ID"   "Vodic za Novi Sad"           "Sta posetiti za jedan dan u Novom Sadu.")
ok "blogovi: B1(autor)=$B1, B2(ana)=$B2"

say "4) Graf pracenja (Neo4j)"
follow "$JOVANA_ID" "$AUTOR_ID";  follow "$JOVANA_ID" "$ANA_ID"
follow "$STEFAN_ID" "$AUTOR_ID";  follow "$TURISTA_ID" "$AUTOR_ID"
follow "$AUTOR_ID"  "$ANA_ID";    follow "$ANA_ID" "$NIKOLA_ID"
ok "jovana->{autor,ana}, stefan->autor, turista->autor, autor->ana, ana->nikola"
ok "preporuka za jovanu (prati autora koji prati anu...): $(curl -s "$G/api/followers/users/recommendations/$JOVANA_ID")"

say "5) Kupovine (token kupovine)"
[ "$T1" != "0" ] && buy_tour "$TURISTA_TOK" "$TURISTA_ID" "$T1" "Beogradska tvrdjava" 1200 && ok "turista kupio T1"
[ "$T3" != "0" ] && buy_tour "$JOVANA_TOK"  "$JOVANA_ID"  "$T3" "Novi Sad i Petrovaradin" 1500 && ok "jovana kupila T3"

say "6) Komentari (samo ako prati autora — tacka 9)"
# jovana prati autora -> sme komentar na B1
curl -s -X POST "$G/api/blogs/$B1/comments" -H "Authorization: Bearer $JOVANA_TOK" -H 'Content-Type: application/json' \
  -d "{\"context\":\"Sjajan vodic, hvala!\",\"author\":\"x\",\"creationTime\":\"2026-06-11T09:00:00Z\",\"lastUpdateTime\":\"2026-06-11T09:00:00Z\",\"userId\":$JOVANA_ID}" -o /dev/null -w "  jovana->B1 komentar [HTTP %{http_code}]\n"
# stefan NE prati anu -> komentar na B2 treba 403
curl -s -X POST "$G/api/blogs/$B2/comments" -H "Authorization: Bearer $STEFAN_TOK" -H 'Content-Type: application/json' \
  -d "{\"context\":\"Test\",\"author\":\"x\",\"creationTime\":\"2026-06-11T10:00:00Z\",\"lastUpdateTime\":\"2026-06-11T10:00:00Z\",\"userId\":$STEFAN_ID}" -o /dev/null -w "  stefan->B2 komentar (ne prati anu) [HTTP %{http_code}]\n"

say "GOTOVO. Pregled:"
echo "  Objavljene ture: $(curl -s "$G/api/tour?page=1&pageSize=50" | $JQ '[.results[] | select(.status==1)] | length')"
echo "  Blogovi: $(curl -s "$G/api/blogs/?page=1&pageSize=50" | $JQ '.totalCount')"
