// app.config.ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { routes } from './app.routes';
import { AuthGuard } from './auth-guard';
import { userReducer } from './ngrx/user.reducer';
import { provideState, provideStore } from '@ngrx/store';
import { BulkInsertService } from './services/BulkInsertService';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withFetch()),
    provideStore(),
    provideState('user', userReducer),
    AuthGuard,
    BulkInsertService
  ]
};