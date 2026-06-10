#!/usr/bin/env bash
# Adds more DOMAIN test data (no new users) to the running stack via the gateway.
# Safe to run multiple times (creates additional tours/blogs/relationships).
set -uo pipefail
G="${GATEWAY:-http://localhost:8080}"; JQ=jq
say(){ printf "\n\033[1m%s\033[0m\n" "$*"; }; ok(){ printf "  ✓ %s\n" "$*"; }

login(){ curl -s -X POST "$G/api/users/login" -H 'Content-Type: application/json' \
  -d "{\"username\":\"$1\",\"password\":\"$2\"}" | $JQ -r '"\(.id) \(.accessToken)"'; }

published_tour(){ # tok author name desc diff price tags -> tourId
  local tok=$1 a=$2 n=$3 d=$4 df=$5 pr=$6 tg=$7 id
  id=$(curl -s -X POST "$G/api/tour" -H "Authorization: Bearer $tok" -H 'Content-Type: application/json' \
    -d "{\"authorId\":$a,\"name\":\"$n\",\"description\":\"$d\",\"difficult\":$df,\"status\":0,\"price\":0,\"tags\":\"$tg\",\"distance\":0}" | $JQ -r '.id')
  [ "$id" = "null" ] && { echo 0; return; }
  curl -s -X PUT "$G/api/tour/updatetour" -H "Authorization: Bearer $tok" -H 'Content-Type: application/json' \
    -d "{\"id\":$id,\"authorId\":$a,\"name\":\"$n\",\"description\":\"$d\",\"difficult\":$df,\"status\":0,\"price\":$pr,\"tags\":\"$tg\",\"distance\":4.5,\"travelTimeAndMethod\":[{\"travelTime\":60,\"travelMethod\":2},{\"travelTime\":25,\"travelMethod\":1}],\"tourEquipment\":[]}" >/dev/null
  for cp in "Start|44.81|20.45" "Sredina|44.82|20.46" "Kraj|44.83|20.47"; do
    IFS='|' read -r cn la lo <<< "$cp"
    curl -s -X POST "$G/api/tour/addCheckpoint" -H "Authorization: Bearer $tok" -H 'Content-Type: application/json' \
      -d "{\"name\":\"$cn\",\"description\":\"Tacka $cn\",\"pictureURL\":\"$cn.jpg\",\"latitude\":$la,\"longitude\":$lo,\"tourId\":$id,\"publicRequest\":{\"status\":0,\"comment\":\"\"}}" >/dev/null
  done
  curl -s -X PUT "$G/api/tour/publishTour" -H "Authorization: Bearer $tok" -H 'Content-Type: application/json' -d "$id" >/dev/null
  echo "$id"
}
make_blog(){ curl -s -X POST "$G/api/blogs/" -H "Authorization: Bearer $1" -H 'Content-Type: application/json' \
  -d "{\"name\":\"$3\",\"description\":\"$4\",\"dateCreated\":\"2026-06-09T10:00:00Z\",\"images\":[],\"authorId\":$2,\"author\":\"x\",\"status\":1,\"rating\":0,\"comments\":[],\"ratings\":[]}" | $JQ -r '.id'; }
rate(){ curl -s -X POST "$G/api/blogs/$2/ratings" -H "Authorization: Bearer $1" -H 'Content-Type: application/json' \
  -d "{\"userId\":$3,\"author\":\"x\",\"ratingType\":$4}" -o /dev/null -w ''; }
comment(){ curl -s -X POST "$G/api/blogs/$2/comments" -H "Authorization: Bearer $1" -H 'Content-Type: application/json' \
  -d "{\"context\":\"$4\",\"author\":\"x\",\"creationTime\":\"2026-06-11T09:00:00Z\",\"lastUpdateTime\":\"2026-06-11T09:00:00Z\",\"userId\":$3}" -o /dev/null -w '%{http_code}'; }
follow(){ curl -s -X POST "$G/api/followers/users" -H 'Content-Type: application/json' -d "{\"followerId\":$1,\"followedId\":$2}" -o /dev/null -w ''; }
buy(){ curl -s -X PUT "$G/api/shoppingCart/addTour/$2" -H "Authorization: Bearer $1" -H 'Content-Type: application/json' \
  -d "{\"tourId\":$3,\"tourName\":\"$4\",\"price\":$5}" -o /dev/null; \
  curl -s -X POST "$G/api/order/$2" -H "Authorization: Bearer $1" -H 'Content-Type: application/json' -d "[$3]" -o /dev/null; }
start_tour(){ curl -s -X POST "$G/api/tourist/execution/tourExecution" -H "Authorization: Bearer $1" -H 'Content-Type: application/json' -d "$2" -w ' [HTTP %{http_code}]'; }
set_location(){ # tok userId lat long
  local p; p=$(curl -s "$G/api/person/$2")
  [ -z "$p" ] || [ "$p" = "null" ] && return
  local body; body=$(echo "$p" | $JQ -c ".latitude=$3 | .longitude=$4")
  curl -s -X PUT "$G/api/person/updateLocation" -H "Authorization: Bearer $1" -H 'Content-Type: application/json' -d "$body" -o /dev/null; }

# ---------------------------------------------------------------------------
say "Korisnici (login postojecih)"
read -r AUTOR A_TOK   < <(login autor autor)
read -r ANA   ANA_TOK < <(login ana ana123)
read -r NIK   N_TOK   < <(login nikola nikola123)
read -r TUR   T_TOK   < <(login turista turista)
read -r JOV   J_TOK   < <(login jovana jovana123)
read -r STE   S_TOK   < <(login stefan stefan123)
ok "autor=$AUTOR ana=$ANA nikola=$NIK | turista=$TUR jovana=$JOV stefan=$STE"

say "Nove objavljene ture (sa 3 kljucne tacke)"
TA=$(published_tour "$A_TOK" "$AUTOR" "Zemunski kej setnja" "Setnja uz Dunav i Gardos" 0 800 "setnja,reka,grad"); ok "Zemunski kej = $TA"
TB=$(published_tour "$A_TOK" "$AUTOR" "Avala i Toranj"      "Spomenik i vidikovac"     1 1000 "priroda,vidikovac"); ok "Avala = $TB"
TC=$(published_tour "$ANA_TOK" "$ANA" "Fruska gora vinski put" "Manastiri i vinarije"  1 2000 "vino,priroda,istorija"); ok "Fruska gora = $TC"
TD=$(published_tour "$N_TOK" "$NIK" "Tara - Banjska stena"  "Planinarska tura"          2 2500 "planina,priroda"); ok "Tara = $TD"

say "Novi blogovi"
BA=$(make_blog "$A_TOK" "$AUTOR" "Skrivene kafane Beograda" "Mesta van turistickih ruta."); ok "blog autor = $BA"
BB=$(make_blog "$ANA_TOK" "$ANA" "Vinski podrumi Vojvodine" "Preporuke za vinske ture."); ok "blog ana = $BB"
BC=$(make_blog "$N_TOK" "$NIK" "Planinarenje za pocetnike"  "Oprema i saveti."); ok "blog nikola = $BC"

say "Lajkovi / ocene blogova (Upwote=0, Downvote=1)"
rate "$T_TOK" "$BA" "$TUR" 0; rate "$J_TOK" "$BA" "$JOV" 0; rate "$S_TOK" "$BA" "$STE" 0
rate "$J_TOK" "$BB" "$JOV" 0; rate "$T_TOK" "$BB" "$TUR" 1
rate "$S_TOK" "$BC" "$STE" 0
ok "BA +3, BB +1/-1, BC +1"

say "Prosirivanje grafa pracenja"
follow "$STE" "$ANA"; follow "$JOV" "$NIK"; follow "$TUR" "$ANA"; follow "$STE" "$NIK"; follow "$JOV" "$STE"
ok "stefan->{ana,nikola}, jovana->{nikola,stefan}, turista->ana"
ok "preporuke turista=$(curl -s "$G/api/followers/users/recommendations/$TUR")  jovana=$(curl -s "$G/api/followers/users/recommendations/$JOV")"

say "Komentari (posle pracenja)"
echo "  stefan->ana blog (prati): [HTTP $(comment "$S_TOK" "$BB" "$STE" "Odlicne preporuke!")]"
echo "  turista->autor blog (prati): [HTTP $(comment "$T_TOK" "$BA" "$TUR" "Svaka cast!")]"
echo "  jovana->nikola blog (prati): [HTTP $(comment "$J_TOK" "$BC" "$JOV" "Bas korisno za pocetnike.")]"

say "Kupovine"
[ "$TA" != 0 ] && buy "$T_TOK" "$TUR" "$TA" "Zemunski kej setnja" 800  && ok "turista kupio Zemunski kej"
[ "$TB" != 0 ] && buy "$S_TOK" "$STE" "$TB" "Avala i Toranj"      1000 && ok "stefan kupio Avalu"
[ "$TC" != 0 ] && buy "$J_TOK" "$JOV" "$TC" "Fruska gora vinski put" 2000 && ok "jovana kupila Frusku goru"

say "Izvodjenja tura (start, tacka 17)"
[ "$TA" != 0 ] && echo "  turista start Zemunski kej:$(start_tour "$T_TOK" "$TA")"
[ "$TC" != 0 ] && echo "  jovana start Fruska gora:$(start_tour "$J_TOK" "$TC")"

say "Lokacije turista (simulator pozicije, tacka 14)"
set_location "$T_TOK" "$TUR" 44.8170 20.4600 && ok "turista lokacija postavljena"
set_location "$J_TOK" "$JOV" 45.2671 19.8335 && ok "jovana lokacija postavljena (Novi Sad)"

say "PREGLED"
echo "  Objavljene ture: $(curl -s "$G/api/tour?page=1&pageSize=100" | $JQ '[.results[]|select(.status==1)]|length')"
echo "  Blogovi:         $(curl -s "$G/api/blogs/?page=1&pageSize=100" | $JQ '.totalCount')"
