import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService, Product } from '../services/product.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css'
})
export class ProductDetailComponent implements OnInit {
  product: any = null;
  loading = true;
  productId: number = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.productId = +params['id'];
      this.loadProductDetail();
    });
  }

  loadProductDetail(): void {
    this.loading = true;

    this.productService.getProductById(this.productId).subscribe({
      next: (data: any) => {
        this.product = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading product:', error);
        this.product = null;
        this.loading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/products']);
  }
}
