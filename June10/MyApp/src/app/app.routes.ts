import { Routes } from '@angular/router';
import { First } from './first/first';
import { Login } from './login/login';
import { Products } from './products/products';
import { Home } from './home/home';
import { Profile } from './profile/profile';
import { AuthGuard } from './auth-guard';
import { UserList } from './user-list/user-list';

export const routes: Routes = [
    { path: '', redirectTo: '/login', pathMatch: 'full' }, // Add this line
    { path: 'landing', component: First },
    { path: 'login', component: Login },
    { path: 'products', component: Products },
    { path: 'home/:un', component: Home, children: [
        { path: 'products', component: Products },
        { path: 'first', component: First }
    ]},
    { path: 'profile', component: Profile, canActivate: [AuthGuard] },
    {path:'users',component:UserList}
];