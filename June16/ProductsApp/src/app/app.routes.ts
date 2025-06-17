import { Routes } from '@angular/router';
import { HomeComponent } from './home/home';
import { AboutComponent } from './about/about';
import { ProductDetailComponent } from './product-detail/product-detail';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/products', pathMatch: 'full' },
  { 
    path: 'products', 
    component: HomeComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'products/:id', 
    component: ProductDetailComponent, 
    canActivate: [AuthGuard] 
  },
  { path: 'about', component: AboutComponent },
  { path: 'home', redirectTo: '/products' },
  { path: '**', redirectTo: '/products' }
];