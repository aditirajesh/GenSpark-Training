import { Component, HostListener, OnInit } from '@angular/core';
import { ProductService, Product } from '../services/product.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { debounceTime, distinctUntilChanged, Subject, switchMap, tap } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './home.html',
  styleUrl: './home.css',
  imports: [FormsModule, CommonModule]
})
export class HomeComponent implements OnInit {
  products: Product[] = [];
  searchString: string = "";
  searchSubject = new Subject<string>();
  loading: boolean = false;
  limit = 10;
  skip = 0;
  total = 0;

  constructor(private productService: ProductService) {}

  handleSearchProducts() {
    this.searchSubject.next(this.searchString);
  }

  ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(1000), 
      distinctUntilChanged(),
      tap(() => this.loading = true),
      switchMap(query => this.productService.searchProducts(query, this.limit, this.skip)),
      tap(() => this.loading = false)
    ).subscribe({
      next: (data: any) => {
        this.products = data.products as Product[];
        this.total = data.total;
        console.log(this.total);
      }
    });

    this.loading = true;
    this.productService.searchProducts('', this.limit, 0).subscribe({
      next: (data: any) => {
        this.products = data.products as Product[];
        this.total = data.total;
        this.loading = false;
      }
    });
  }

  @HostListener('window:scroll', [])
  onScroll(): void {
    const scrollPosition = window.innerHeight + window.scrollY;
    const threshold = document.body.offsetHeight - 100;
    if (scrollPosition >= threshold && this.products?.length < this.total) {
      console.log(scrollPosition);
      console.log(threshold);
      this.loadMore();
    }
  }

  loadMore() {
    this.loading = true;
    this.skip += this.limit;
    this.productService.searchProducts(this.searchString, this.limit, this.skip)
      .subscribe({
        next: (data: any) => {

          this.products = [...this.products, ...data.products];
          this.loading = false;
        }
      });
  }
}