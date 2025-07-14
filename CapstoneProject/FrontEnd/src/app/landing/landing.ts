import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-landing',
  imports: [CommonModule],
  templateUrl: './landing.html',
  styleUrl: './landing.css'
})
export class Landing {
  constructor(private router: Router) {}
  navigateToSignup(): void {
    this.router.navigate(['/signup']);
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }
  
}
