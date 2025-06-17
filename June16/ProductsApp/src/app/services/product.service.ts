import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: number;
  title: string;
  price: number;
  thumbnail: string;
}

export interface ProductResponse {
  products: Product[];
  total: number;
  skip: number;
  limit: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private baseUrl = 'https://dummyjson.com/products/search';

  constructor(private http: HttpClient) { }

  searchProducts(query: string = '', limit: number = 10, skip: number = 10): Observable<ProductResponse> {
    const url = `${this.baseUrl}?q=${encodeURIComponent(query)}&limit=${limit}&skip=${skip}`;
    return this.http.get<ProductResponse>(url);
  }
}
