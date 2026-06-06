import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AnswerDate,
  Questionnaire,
  UserProfile,
} from './model/userProfile.model';

@Injectable({
  providedIn: 'root',
})
export class LayoutService {
  constructor(private http: HttpClient) {}

  getProfile(id: number): Observable<UserProfile> {
    return this.http.get<UserProfile>(
      `http://localhost:8080/api/person/${id}`
    );
  }

  updateProfile(
    profile: UserProfile,
    imageFile: File | null
  ): Observable<UserProfile> {
    const formData = new FormData();

    formData.append('id', profile.id.toString());
    formData.append('userId', profile.userId.toString());
    formData.append('name', profile.name);
    formData.append('surname', profile.surname);
    formData.append('email', profile.email);
    formData.append('motto', profile.motto);
    formData.append('biography', profile.biography);
    // Image is optional. Send the new file if one was picked; otherwise resend the
    // existing filename so it is preserved. If there is neither, omit it entirely
    // (an empty Image field is rejected by the backend).
    if (imageFile) {
      formData.append('Image', imageFile, imageFile.name);
    } else if (profile.image) {
      formData.append('Image', profile.image.toString());
    }

    return this.http.put<UserProfile>(
      'http://localhost:8080/api/person/',
      formData
    );
  }

  getQuestion(): Observable<Questionnaire> {
    return this.http.get<Questionnaire>(
      'http://localhost:8080/api/questionnaire/getQuestion'
    );
  }

  newAnswerDate(userId: number): Observable<AnswerDate> {
    return this.http.post<AnswerDate>(
      `http://localhost:8080/api/questionnaire/createOrUpdateLastAnswerDate/${userId}`,
      null
    );
  }

  getLastAnswerDate(userId: number | undefined): Observable<AnswerDate> {
    return this.http.get<AnswerDate>(
      `http://localhost:8080/api/questionnaire/getLastAnswerDate/${userId}`
    );
  }
}
