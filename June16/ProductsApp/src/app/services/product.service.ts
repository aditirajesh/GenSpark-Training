import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: number;
  title: string;
  price: number;
  thumbnail: string;
  description?: string;
  category?: string;
  brand?: string;
  stock?: number;
  rating?: number;
  discountPercentage?: number;
  images?: string[];
} //how the response from the api should look like - for getting product by id

export interface ProductResponse {
  products: Product[];
  total: number;
  skip: number;
  limit: number;
} //search response 

@Injectable({
  providedIn: 'root'
}) //making sure this service is available throughout the app

export class ProductService {
  private baseUrl = 'https://dummyjson.com/products/search';
  private productUrl = 'https://dummyjson.com/products';

  constructor(private http: HttpClient) { }

  searchProducts(query: string = '', limit: number = 10, skip: number = 10): Observable<ProductResponse> {
    const url = `${this.baseUrl}?q=${encodeURIComponent(query)}&limit=${limit}&skip=${skip}`;
    return this.http.get<ProductResponse>(url);
  }

  getProductById(id: number): Observable<Product> {
    const url = `${this.productUrl}/${id}`;
    return this.http.get<Product>(url);
  }
}
