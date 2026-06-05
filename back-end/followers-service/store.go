package main

import (
	"context"
	"fmt"

	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
)

// Store wraps a Neo4j graph holding (:User)-[:FOLLOWS]->(:User) relationships.
type Store struct {
	driver neo4j.DriverWithContext
}

func NewStore(uri, user, password string) (*Store, error) {
	driver, err := neo4j.NewDriverWithContext(uri, neo4j.BasicAuth(user, password, ""))
	if err != nil {
		return nil, err
	}
	if err := driver.VerifyConnectivity(context.Background()); err != nil {
		return nil, err
	}
	return &Store{driver: driver}, nil
}

func (s *Store) Close(ctx context.Context) {
	_ = s.driver.Close(ctx)
}

// Follow creates a FOLLOWS edge from follower to followed (idempotent).
func (s *Store) Follow(ctx context.Context, followerId, followedId int64) error {
	if followerId == followedId {
		return fmt.Errorf("a user cannot follow themselves")
	}
	session := s.driver.NewSession(ctx, neo4j.SessionConfig{})
	defer session.Close(ctx)

	_, err := session.ExecuteWrite(ctx, func(tx neo4j.ManagedTransaction) (any, error) {
		return tx.Run(ctx,
			`MERGE (a:User {id: $follower})
			 MERGE (b:User {id: $followed})
			 MERGE (a)-[:FOLLOWS]->(b)`,
			map[string]any{"follower": followerId, "followed": followedId})
	})
	return err
}

// Unfollow removes the FOLLOWS edge if present.
func (s *Store) Unfollow(ctx context.Context, followerId, followedId int64) error {
	session := s.driver.NewSession(ctx, neo4j.SessionConfig{})
	defer session.Close(ctx)

	_, err := session.ExecuteWrite(ctx, func(tx neo4j.ManagedTransaction) (any, error) {
		return tx.Run(ctx,
			`MATCH (a:User {id: $follower})-[r:FOLLOWS]->(b:User {id: $followed}) DELETE r`,
			map[string]any{"follower": followerId, "followed": followedId})
	})
	return err
}

// IsFollowing reports whether follower follows followed.
func (s *Store) IsFollowing(ctx context.Context, followerId, followedId int64) (bool, error) {
	session := s.driver.NewSession(ctx, neo4j.SessionConfig{})
	defer session.Close(ctx)

	res, err := session.ExecuteRead(ctx, func(tx neo4j.ManagedTransaction) (any, error) {
		r, err := tx.Run(ctx,
			`RETURN EXISTS( (:User {id: $follower})-[:FOLLOWS]->(:User {id: $followed}) ) AS following`,
			map[string]any{"follower": followerId, "followed": followedId})
		if err != nil {
			return nil, err
		}
		rec, err := r.Single(ctx)
		if err != nil {
			return false, nil
		}
		return rec.Values[0].(bool), nil
	})
	if err != nil {
		return false, err
	}
	return res.(bool), nil
}

// GetFollowing returns the ids the user follows.
func (s *Store) GetFollowing(ctx context.Context, userId int64) ([]int64, error) {
	return s.collectIds(ctx,
		`MATCH (:User {id: $id})-[:FOLLOWS]->(f:User) RETURN f.id AS id ORDER BY id`,
		map[string]any{"id": userId})
}

// GetFollowers returns the ids that follow the user.
func (s *Store) GetFollowers(ctx context.Context, userId int64) ([]int64, error) {
	return s.collectIds(ctx,
		`MATCH (f:User)-[:FOLLOWS]->(:User {id: $id}) RETURN f.id AS id ORDER BY id`,
		map[string]any{"id": userId})
}

// GetRecommendations suggests profiles followed by the people the user follows,
// excluding the user and anyone they already follow (functionality 9.3).
func (s *Store) GetRecommendations(ctx context.Context, userId int64) ([]int64, error) {
	return s.collectIds(ctx,
		`MATCH (u:User {id: $id})-[:FOLLOWS]->(:User)-[:FOLLOWS]->(rec:User)
		 WHERE rec.id <> $id AND NOT (u)-[:FOLLOWS]->(rec)
		 RETURN DISTINCT rec.id AS id ORDER BY id`,
		map[string]any{"id": userId})
}

func (s *Store) collectIds(ctx context.Context, cypher string, params map[string]any) ([]int64, error) {
	session := s.driver.NewSession(ctx, neo4j.SessionConfig{})
	defer session.Close(ctx)

	res, err := session.ExecuteRead(ctx, func(tx neo4j.ManagedTransaction) (any, error) {
		r, err := tx.Run(ctx, cypher, params)
		if err != nil {
			return nil, err
		}
		ids := []int64{}
		for r.Next(ctx) {
			ids = append(ids, r.Record().Values[0].(int64))
		}
		return ids, r.Err()
	})
	if err != nil {
		return nil, err
	}
	return res.([]int64), nil
}
