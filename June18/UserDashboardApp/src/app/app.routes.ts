import { Routes } from "@angular/router";
import { DashboardComponent } from "./dashboard/dashboard";

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: '**', redirectTo: '' }
];