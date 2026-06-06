import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/env/environment';

// Aligned with the Go Followers microservice (Neo4j-backed) exposed through the gateway.
@Injectable({
  providedIn: 'root'
})
export class FollowersService {

  private base = environment.apiHost + 'followers/users';

  constructor(private http: HttpClient) { }

  // Ids of the users that the given user follows.
  getFollowing(userId: number): Observable<number[]> {
    return this.http.get<number[]>(`${this.base}/following/${userId}`);
  }

  // Ids of the users that follow the given user.
  getFollowers(userId: number): Observable<number[]> {
    return this.http.get<number[]>(`${this.base}/followers/${userId}`);
  }

  // Suggested profiles (followed by the people the user follows).
  getRecommendations(userId: number): Observable<number[]> {
    return this.http.get<number[]>(`${this.base}/recommendations/${userId}`);
  }

  follow(followerId: number, followedId: number): Observable<any> {
    return this.http.post(this.base, { followerId, followedId });
  }

  unfollow(followerId: number, followedId: number): Observable<any> {
    return this.http.delete(`${this.base}/${followerId}/${followedId}`);
  }

  isFollowing(followerId: number, followedId: number): Observable<{ isFollowing: boolean }> {
    return this.http.get<{ isFollowing: boolean }>(`${this.base}/isFollowing/${followerId}/${followedId}`);
  }

  // Username resolution via the gateway gRPC endpoint (gateway -> Stakeholders).
  getUsername(userId: number): Observable<{ id: number; username: string }> {
    return this.http.get<{ id: number; username: string }>(`${environment.apiHost}rpc/users/${userId}`);
  }

  // All users (so the current user can follow anyone, not just suggestions).
  getAllUsers(): Observable<{ results: { id: number; username: string; role: number }[]; totalCount: number }> {
    return this.http.get<{ results: { id: number; username: string; role: number }[]; totalCount: number }>(
      `${environment.apiHost}users/all`
    );
  }
}
