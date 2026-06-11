import { Component, OnInit } from '@angular/core';

import * as paginator from '@angular/material/paginator';
import { TourAuthoringService } from '../../tour-authoring/tour-authoring.service';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { PagedResults } from 'src/app/shared/model/paged-results.model';
import { DatePipe } from '@angular/common';
import { AvailableTour } from '../model/available-tour-model';
import { Router } from '@angular/router';
import { MarketplaceService } from '../marketplace.service';
import { CheckpointService } from '../../tour-authoring/checkpoint.service';

@Component({
  selector: 'xp-tourist-bought-tours',
  templateUrl: './tourist-bought-tours.component.html',
  styleUrls: ['./tourist-bought-tours.component.css'],
  providers: [DatePipe],
})
export class TouristBoughtToursComponent implements OnInit {
  user: User;
  availableTours: AvailableTour[] = [];

  constructor(
    private tourService: TourAuthoringService,
    private authService: AuthService,
    private marketplaceService: MarketplaceService,
    private checkpointService: CheckpointService,
    private datePipe: DatePipe,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.user$.subscribe((user) => {
      this.user = user;
      if (user && user.id) this.getTouristTours(user.id);
    });
  }

  // Purchased tours live in the Purchase microservice; tour details in Tours.
  // Compose them: orders -> tourIds -> tour details (+ checkpoints).
  getTouristTours(userId: number) {
    this.marketplaceService.getTouristOrders(userId).subscribe({
      next: (orders) => {
        const tourIds = Array.from(new Set(orders.results.map((o) => o.tourId)));
        this.availableTours = [];
        tourIds.forEach((tourId) => {
          this.tourService.getTourById(tourId).subscribe({
            next: (tour: any) => {
              this.availableTours.push(tour);
              this.checkpointService.getCheckpoints(tourId).subscribe({
                next: (cps) => { tour.checkpoints = cps.results; },
              });
            },
            error: (err: any) => console.error(err),
          });
        });
      },
      error: (err: any) => console.error(err),
    });
  }

  navigateToTourExecution(tourId: number) {
    this.router.navigate([`position-simulator/${tourId}`]);
  }

  formatDate(date: string | Date): string {
    return this.datePipe.transform(date, 'dd.MM.yyyy HH:mm:ss') || '';
  }
}
