import { Component, EventEmitter, Inject, OnInit, OnDestroy, Output } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { Followers } from './followers.model';
import { FollowersService } from './followers.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import Swal from 'sweetalert2';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'xp-followers-dialog',
  templateUrl: './followers-dialog.component.html',
  styleUrls: ['./followers-dialog.component.css']
})
export class FollowersDialogComponent implements OnInit, OnDestroy {

  user: User | undefined;
  userId: number;

  followers: Followers[] = [];     // users who follow me
  following: Followers[] = [];     // users I follow
  recommendations: Followers[] = [];
  allUsers: { id: number; username: string }[] = [];  // everyone (minus me)

  followersUsernames: { [key: number]: string } = {};
  followingUsernames: { [key: number]: string } = {};
  recommendationsUsernames: { [key: number]: string } = {};

  private followingIds = new Set<number>();

  showFollowers = true;
  showFollowing = false;
  showRecommendations = false;
  showAllUsers = false;

  @Output() refreshFollowers = new EventEmitter<boolean>();

  constructor(public dialogRef: MatDialogRef<FollowersDialogComponent>,
    private followersService: FollowersService,
    private authService: AuthService,
    private toastr: ToastrService,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    if (data && data.type === 'following') {
      this.toggleFollowing();
    }
  }

  ngOnInit(): void {
    this.userId = this.authService.user$.getValue().id;
    this.authService.user$.subscribe((user) => { this.user = user; });
    this.refresh();
  }

  ngOnDestroy(): void {
    this.dialogRef.close({ refreshFollowers: true });
  }

  private refresh(): void {
    this.loadFollowing();
    this.loadFollowers();
    this.loadRecommendations();
    this.loadAllUsers();
  }

  loadAllUsers(): void {
    this.followersService.getAllUsers().subscribe({
      next: (res) => {
        // Everyone except myself; the template shows Follow/Unfollow per row.
        this.allUsers = res.results
          .filter(u => u.id !== this.userId)
          .map(u => ({ id: u.id, username: u.username }));
      }
    });
  }

  private resolveUsername(id: number, target: { [key: number]: string }): void {
    this.followersService.getUsername(id).subscribe({
      next: (res) => { target[id] = res.username; }
    });
  }

  loadFollowing(): void {
    this.followersService.getFollowing(this.userId).subscribe({
      next: (ids) => {
        this.followingIds = new Set(ids);
        this.following = ids.map(id => ({ followingId: this.userId, followedId: id }));
        ids.forEach(id => this.resolveUsername(id, this.followingUsernames));
      }
    });
  }

  loadFollowers(): void {
    this.followersService.getFollowers(this.userId).subscribe({
      next: (ids) => {
        this.followers = ids.map(id => ({ followedId: this.userId, followingId: id }));
        ids.forEach(id => this.resolveUsername(id, this.followersUsernames));
      }
    });
  }

  loadRecommendations(): void {
    this.followersService.getRecommendations(this.userId).subscribe({
      next: (ids) => {
        this.recommendations = ids.map(id => ({ followedId: id, followingId: this.userId }));
        ids.forEach(id => this.resolveUsername(id, this.recommendationsUsernames));
      }
    });
  }

  isFollowing(userId: number): boolean {
    return this.followingIds.has(userId);
  }

  followUser(folId: number): void {
    this.followersService.follow(this.userId, folId).subscribe({
      next: () => {
        this.toastr.success('Followed successfully');
        this.refresh();
      },
      error: (err) => console.error('Error following user: ', err)
    });
  }

  unfollowUser(folId: number): void {
    this.followersService.unfollow(this.userId, folId).subscribe({
      next: () => {
        this.toastr.success('Unfollowed');
        this.refresh();
      }
    });
  }

  unfollowConfirmation(folId: number): void {
    Swal.fire({
      title: 'Are you sure?',
      text: 'You are about to unfollow this user.',
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Unfollow',
      cancelButtonText: 'Cancel'
    }).then((result) => {
      if (result.isConfirmed) {
        this.unfollowUser(folId);
      }
    });
  }

  toggleFollowers(): void {
    this.showFollowers = true; this.showFollowing = false; this.showRecommendations = false; this.showAllUsers = false;
  }

  toggleFollowing(): void {
    this.showFollowers = false; this.showFollowing = true; this.showRecommendations = false; this.showAllUsers = false;
  }

  toggleRecommendations(): void {
    this.showFollowers = false; this.showFollowing = false; this.showRecommendations = true; this.showAllUsers = false;
  }

  toggleAllUsers(): void {
    this.showFollowers = false; this.showFollowing = false; this.showRecommendations = false; this.showAllUsers = true;
  }
}
