package main

import (
	"context"
	"encoding/json"
	"log"
	"net/http"
	"os"
	"strconv"
	"strings"
)

var store *Store

func main() {
	uri := getenv("NEO4J_URI", "bolt://followers-neo4j:7687")
	user := getenv("NEO4J_USER", "neo4j")
	password := getenv("NEO4J_PASSWORD", "superSecret123")

	s, err := NewStore(uri, user, password)
	if err != nil {
		log.Fatalf("failed to connect to Neo4j: %v", err)
	}
	store = s
	defer store.Close(context.Background())

	mux := http.NewServeMux()
	mux.HandleFunc("/health", func(w http.ResponseWriter, r *http.Request) {
		writeJSON(w, http.StatusOK, map[string]string{"status": "followers up"})
	})
	// Base collection: POST = follow, DELETE = unfollow.
	mux.HandleFunc("/api/followers/users", handleFollowRoot)
	// Sub-resources: following / followers / recommendations / isFollowing.
	mux.HandleFunc("/api/followers/users/", handleFollowSub)

	addr := ":8080"
	log.Printf("followers service listening on %s", addr)
	if err := http.ListenAndServe(addr, withCORS(mux)); err != nil {
		log.Fatal(err)
	}
}

type followRequest struct {
	FollowerId int64 `json:"followerId"`
	FollowedId int64 `json:"followedId"`
}

func handleFollowRoot(w http.ResponseWriter, r *http.Request) {
	ctx := r.Context()
	switch r.Method {
	case http.MethodPost:
		var req followRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeError(w, http.StatusBadRequest, "invalid body")
			return
		}
		if err := store.Follow(ctx, req.FollowerId, req.FollowedId); err != nil {
			writeError(w, http.StatusBadRequest, err.Error())
			return
		}
		writeJSON(w, http.StatusOK, req)
	case http.MethodDelete:
		var req followRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeError(w, http.StatusBadRequest, "invalid body")
			return
		}
		if err := store.Unfollow(ctx, req.FollowerId, req.FollowedId); err != nil {
			writeError(w, http.StatusInternalServerError, err.Error())
			return
		}
		writeJSON(w, http.StatusOK, req)
	default:
		writeError(w, http.StatusMethodNotAllowed, "method not allowed")
	}
}

// Routes:
//   GET    /api/followers/users/following/{userId}
//   GET    /api/followers/users/followers/{userId}
//   GET    /api/followers/users/recommendations/{userId}
//   GET    /api/followers/users/isFollowing/{followerId}/{followedId}
//   DELETE /api/followers/users/{followerId}/{followedId}
func handleFollowSub(w http.ResponseWriter, r *http.Request) {
	ctx := r.Context()
	path := strings.TrimPrefix(r.URL.Path, "/api/followers/users/")
	parts := strings.Split(strings.Trim(path, "/"), "/")

	switch parts[0] {
	case "following", "followers", "recommendations":
		if len(parts) != 2 {
			writeError(w, http.StatusBadRequest, "expected a user id")
			return
		}
		id, err := strconv.ParseInt(parts[1], 10, 64)
		if err != nil {
			writeError(w, http.StatusBadRequest, "invalid user id")
			return
		}
		var ids []int64
		switch parts[0] {
		case "following":
			ids, err = store.GetFollowing(ctx, id)
		case "followers":
			ids, err = store.GetFollowers(ctx, id)
		case "recommendations":
			ids, err = store.GetRecommendations(ctx, id)
		}
		if err != nil {
			writeError(w, http.StatusInternalServerError, err.Error())
			return
		}
		writeJSON(w, http.StatusOK, ids)

	case "isFollowing":
		if len(parts) != 3 {
			writeError(w, http.StatusBadRequest, "expected followerId/followedId")
			return
		}
		a, err1 := strconv.ParseInt(parts[1], 10, 64)
		b, err2 := strconv.ParseInt(parts[2], 10, 64)
		if err1 != nil || err2 != nil {
			writeError(w, http.StatusBadRequest, "invalid ids")
			return
		}
		following, err := store.IsFollowing(ctx, a, b)
		if err != nil {
			writeError(w, http.StatusInternalServerError, err.Error())
			return
		}
		writeJSON(w, http.StatusOK, map[string]bool{"isFollowing": following})

	default:
		// /{followerId}/{followedId} DELETE unfollow
		if r.Method == http.MethodDelete && len(parts) == 2 {
			a, err1 := strconv.ParseInt(parts[0], 10, 64)
			b, err2 := strconv.ParseInt(parts[1], 10, 64)
			if err1 != nil || err2 != nil {
				writeError(w, http.StatusBadRequest, "invalid ids")
				return
			}
			if err := store.Unfollow(ctx, a, b); err != nil {
				writeError(w, http.StatusInternalServerError, err.Error())
				return
			}
			writeJSON(w, http.StatusOK, map[string]string{"status": "unfollowed"})
			return
		}
		writeError(w, http.StatusNotFound, "not found")
	}
}

func writeJSON(w http.ResponseWriter, status int, body any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(body)
}

func writeError(w http.ResponseWriter, status int, msg string) {
	writeJSON(w, status, map[string]string{"error": msg})
}

func withCORS(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS")
		w.Header().Set("Access-Control-Allow-Headers", "Content-Type, Authorization")
		if r.Method == http.MethodOptions {
			w.WriteHeader(http.StatusOK)
			return
		}
		next.ServeHTTP(w, r)
	})
}

func getenv(key, def string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return def
}
