import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { catchError, Observable, of, throwError } from "rxjs";
import { RecipeModel } from "../models/recipe";

@Injectable({
  providedIn: 'root'
})
export class RecipeService {
  private http = inject(HttpClient);

  getRecipe(id: number = 1): Observable<RecipeModel> {
    return this.http.get<RecipeModel>(`https://dummyjson.com/recipes/${id}`)
      .pipe(catchError((error) => {
        console.error('Error fetching recipe:', error);
        return throwError(() => error);
      }));
  }

  getAllRecipes(): Observable<{ recipes: RecipeModel[], total: number, skip: number, limit: number }> {
  return this.http.get<{ recipes: RecipeModel[], total: number, skip: number, limit: number }>('https://dummyjson.com/recipes')
    .pipe(
      catchError((error) => {
        console.error('Error fetching recipes:', error);
        // Return empty dataset with a flag indicating error
        return of({
          recipes: [],
          total: 0,
          skip: 0,
          limit: 0,
          error: true // Custom property to indicate error occurred
        } as any);
      })
    );
}
}
