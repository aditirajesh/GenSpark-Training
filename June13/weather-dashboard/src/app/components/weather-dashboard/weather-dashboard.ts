import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, Subject, takeUntil } from 'rxjs';

import { WeatherService, WeatherData, WeatherError } from '../../services/weather';
import { CitySearchComponent } from '../city-search/city-search';
import { WeatherCardComponent } from '../weather-card/weather-card';

@Component({
  selector: 'app-weather-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CitySearchComponent,
    WeatherCardComponent
  ],
  templateUrl: './weather-dashboard.html',
  styleUrls: ['./weather-dashboard.css']
})
export class WeatherDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  weatherData$: Observable<WeatherData | null>;
  loading$: Observable<boolean>;
  error$: Observable<WeatherError | null>;

  constructor(private weatherService: WeatherService) {
    this.weatherData$ = this.weatherService.weatherData$;
    this.loading$ = this.weatherService.loading$;
    this.error$ = this.weatherService.error$;
  }

  ngOnInit(): void {
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  clearWeatherData(): void {
    this.weatherService.clearWeatherData();
  }

  searchDemoCity(city: string): void {
    this.weatherService.searchCityWeather(city);
  }

  get isUsingMockData(): boolean {
    return (this.weatherService as any).USE_MOCK_DATA;
  }

  get demoCities(): string[] {
    return ['London', 'New York', 'Tokyo', 'Paris', 'Sydney'];
  }
}