import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { PagedResults } from 'src/app/shared/model/paged-results.model';
import { NotificationModel } from './notification.model';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  constructor(private http: HttpClient, private authService: AuthService) {}

  getNotifications(): Observable<PagedResults<NotificationModel>> {
    return this.http.get<PagedResults<NotificationModel>>(
      'http://localhost:8080/api/notifications/users'
    );
  }

  updateNotification(
    notification: NotificationModel
  ): Observable<NotificationModel> {
    return this.http.put<NotificationModel>(
      'http://localhost:8080/api/notifications/users/' + notification.id,
      notification
    );
  }

  createNotification(
    notification: NotificationModel
  ): Observable<NotificationModel> {
    return this.http.post<NotificationModel>(
      'http://localhost:8080/api/notifications/users',
      notification
    );
  }
}
