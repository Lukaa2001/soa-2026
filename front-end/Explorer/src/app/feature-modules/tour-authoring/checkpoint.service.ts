import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/env/environment';
import { Observable } from 'rxjs';
import { PagedResults } from 'src/app/shared/model/paged-results.model';
import { Checkpoint } from './model/checkpoint.model';

@Injectable({
  providedIn: 'root'
})
export class CheckpointService {

  constructor(private http: HttpClient) { }


  addCheckpoint(tourId : number, checkpoint : Checkpoint): Observable<void> {
    return this.http.post<void>('http://localhost:8080/api/tour/addCheckpoint/', checkpoint);
  }

  getCheckpoints(id : number) : Observable<PagedResults<Checkpoint>> {
    return this.http.get<PagedResults<Checkpoint>>('http://localhost:8080/api/tour/getCheckpoints/' + id);
  }

  getCompositeTourCheckpoints(compositeTourId : number) : Observable<PagedResults<Checkpoint>> {
    return this.http.get<PagedResults<Checkpoint>>('http://localhost:8080/api/compositetour/getCheckpoints/' + compositeTourId);
  }

  editCheckpoint(checkpoint: Checkpoint) : Observable<Checkpoint> {
    return this.http.put<Checkpoint>('http://localhost:8080/api/tour/updateCheckpoint', checkpoint);
  }

  deleteCheckpoint(id: number) : Observable<void> {
    return this.http.delete<void>('http://localhost:8080/api/tour/deleteCheckpoint/' + id);
  }

}