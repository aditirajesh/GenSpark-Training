// src/app/services/weather.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export interface WeatherData {
  city: string;
  temperature: number;
  description: string;
  humidity: number;
  windSpeed: number;
  icon: string;
  country: string;
}

export interface WeatherError {
  message: string;
  type: 'NOT_FOUND' | 'API_ERROR' | 'NETWORK_ERROR';
}

@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  private readonly API_KEY = 'YOUR_API_KEY_HERE'; // Replace with your OpenWeatherMap API key
  private readonly BASE_URL = 'https://api.openweathermap.org/data/2.5/weather';
  private readonly USE_MOCK_DATA = this.API_KEY === 'YOUR_API_KEY_HERE';
  
  // Mock data for testing
  private mockData: { [key: string]: WeatherData } = {
    'london': {
      city: 'London',
      temperature: 15,
      description: 'partly cloudy',
      humidity: 65,
      windSpeed: 3.5,
      icon: '02d',
      country: 'GB'
    },
    'new york': {
      city: 'New York',
      temperature: 22,
      description: 'clear sky',
      humidity: 45,
      windSpeed: 2.1,
      icon: '01d',
      country: 'US'
    },
    'tokyo': {
      city: 'Tokyo',
      temperature: 28,
      description: 'light rain',
      humidity: 78,
      windSpeed: 1.8,
      icon: '10d',
      country: 'JP'
    },
    'paris': {
      city: 'Paris',
      temperature: 18,
      description: 'overcast clouds',
      humidity: 72,
      windSpeed: 2.8,
      icon: '04d',
      country: 'FR'
    },
    'sydney': {
      city: 'Sydney',
      temperature: 25,
      description: 'few clouds',
      humidity: 58,
      windSpeed: 4.2,
      icon: '02d',
      country: 'AU'
    }
  };
  
  // BehaviorSubject for component communication
  private weatherDataSubject = new BehaviorSubject<WeatherData | null>(null);
  private errorSubject = new BehaviorSubject<WeatherError | null>(null);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  // Public observables
  public weatherData$ = this.weatherDataSubject.asObservable();
  public error$ = this.errorSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient) { }

  // Search for city weather
  searchCityWeather(city: string): void {
    if (!city.trim()) {
      this.setError('Please enter a city name', 'NOT_FOUND');
      return;
    }

    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    if (this.USE_MOCK_DATA) {
      this.getMockWeatherData(city);
    } else {
      this.getApiWeatherData(city);
    }
  }

  // Clear current weather data
  clearWeatherData(): void {
    this.weatherDataSubject.next(null);
    this.errorSubject.next(null);
  }

  // Get mock weather data
  private getMockWeatherData(city: string): void {
    setTimeout(() => {
      const normalizedCity = city.toLowerCase().trim();
      const data = this.mockData[normalizedCity];
      
      if (data) {
        this.weatherDataSubject.next(data);
      } else {
        this.setError('City not found in demo data. Try: London, New York, Tokyo, Paris, or Sydney.', 'NOT_FOUND');
      }
      
      this.loadingSubject.next(false);
    }, 1000);
  }

  // Get API weather data
  private getApiWeatherData(city: string): void {
    const url = `${this.BASE_URL}?q=${city}&appid=${this.API_KEY}&units=metric`;

    this.http.get<any>(url).pipe(
      map(response => this.mapApiResponse(response)),
      catchError(error => this.handleError(error))
    ).subscribe({
      next: (data) => {
        this.weatherDataSubject.next(data);
        this.loadingSubject.next(false);
      },
      error: () => {
        this.weatherDataSubject.next(null);
        this.loadingSubject.next(false);
      }
    });
  }

  // Map API response to WeatherData
  private mapApiResponse(response: any): WeatherData {
    return {
      city: response.name,
      temperature: Math.round(response.main.temp),
      description: response.weather[0].description,
      humidity: response.main.humidity,
      windSpeed: response.wind.speed,
      icon: response.weather[0].icon,
      country: response.sys.country
    };
  }

  // Handle errors
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage: WeatherError;

    if (error.status === 404) {
      errorMessage = {
        message: 'City not found. Please check the city name and try again.',
        type: 'NOT_FOUND'
      };
    } else if (error.status === 401) {
      errorMessage = {
        message: 'Invalid API key. Please check your configuration.',
        type: 'API_ERROR'
      };
    } else if (error.status === 0) {
      errorMessage = {
        message: 'Network error. Please check your internet connection.',
        type: 'NETWORK_ERROR'
      };
    } else {
      errorMessage = {
        message: 'An unexpected error occurred. Please try again.',
        type: 'API_ERROR'
      };
    }

    this.setError(errorMessage.message, errorMessage.type);
    return throwError(() => errorMessage);
  }

  private setError(message: string, type: WeatherError['type']): void {
    this.errorSubject.next({ message, type });
  }
}