import { Routes } from '@angular/router';
import { authGuard } from './auth.guard';
import { noAuthGuard } from './no-auth.guard';
import { ExpenseComponent } from './expense/expense';
import { AddExpenseComponent } from './add-expense/add-expense';
import { EditExpenseComponent } from './edit-expense/edit-expense';
import { HomeComponent } from './home/home';
import { LoginComponent } from './login/login';
import { ReportComponent } from './report/report';
import { UserProfileComponent } from './userprofile/userprofile';
import { AdminHomeComponent } from './adminhome/adminhome';
import { AdminGuard } from './admin.guard';
import { RoleRedirectGuard } from './role-redirect.guard';
import { SignupComponent } from './signup/signup';
import { Landing } from './landing/landing';

export const routes: Routes = [
  // Default route - redirect based on authentication
  { 
    path: '', 
    redirectTo: '/landing', 
    pathMatch: 'full' 
  },

  { 
    path: 'signup',
    component: SignupComponent,
    canActivate: [noAuthGuard]
  },

  {
    path:'landing',
    component: Landing,
    canActivate: [noAuthGuard]
  },
  
  // Login route
  { 
    path: 'login', 
    component: LoginComponent 
  },
  
  // Regular user home (with role check to redirect admins)
  { 
    path: 'home', 
    component: HomeComponent, 
    canActivate: [authGuard, RoleRedirectGuard] 
  },
  
  // Admin home (admin only)
  { 
    path: 'admin-home', 
    component: AdminHomeComponent, 
    canActivate: [authGuard, AdminGuard] 
  },
  
  // Shared routes (accessible by both users and admins)
  { 
    path: 'expenses', 
    component: ExpenseComponent, 
    canActivate: [authGuard] 
  },
  { 
    path: 'reports', 
    component: ReportComponent, 
    canActivate: [authGuard] 
  },
  { 
    path: 'profile', 
    component: UserProfileComponent, 
    canActivate: [authGuard] 
  },
  
  // Catch-all route
  { 
    path: '**', 
    redirectTo: '/login' 
  }
];
