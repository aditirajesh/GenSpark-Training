// src/app/components/city-search/city-search.component.ts
import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { WeatherService } from '../../services/weather';

@Component({
  selector: 'app-city-search',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './city-search.html',
  styleUrls: ['./city-search.css']
})
export class CitySearchComponent implements OnInit {
  cityControl = new FormControl('', [Validators.required]);
  loading$;
  hasWeatherData$;

  constructor(private weatherService: WeatherService) {
    this.loading$ = this.weatherService.loading$;
    this.hasWeatherData$ = this.weatherService.weatherData$;
  }

  ngOnInit(): void {
  }

  onSearch(): void {
    if (this.cityControl.valid && this.cityControl.value) {
      this.weatherService.searchCityWeather(this.cityControl.value.trim());
    }
  }

  onClear(): void {
    this.cityControl.reset();
    this.weatherService.clearWeatherData();
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.onSearch();
    }
  }
}