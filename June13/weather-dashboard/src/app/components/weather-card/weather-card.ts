// src/app/components/weather-card/weather-card.component.ts
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WeatherData } from '../../services/weather';

@Component({
  selector: 'app-weather-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './weather-card.html',
  styleUrls: ['./weather-card.css']
})
export class WeatherCardComponent {
  @Input() weatherData: WeatherData | null = null;

  getWeatherIconUrl(iconCode: string): string {
    return `https://openweathermap.org/img/wn/${iconCode}@2x.png`;
  }

  capitalizeDescription(description: string): string {
    return description.charAt(0).toUpperCase() + description.slice(1);
  }
}