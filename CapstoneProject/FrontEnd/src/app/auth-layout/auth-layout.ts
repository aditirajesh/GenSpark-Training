import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  imports: [CommonModule],
  templateUrl: './auth-layout.html',
  styleUrl: './auth-layout.css'
})
export class AuthLayoutComponent {
  @Input() heroTitle: string = 'Welcome';
  @Input() heroSubtitle: string = 'Access your account';

  constructor(private router: Router) {}

  navigateToLanding(): void {
    this.router.navigate(['/']);
  }
}
