import { Component, OnInit, signal } from '@angular/core';
import { RecipeModel } from '../models/recipe';
import { RecipeService } from '../services/recipe.service';
import { Recipe } from "../recipe/recipe";

@Component({
  selector: 'app-recipies',
  imports: [Recipe],
  templateUrl: './recipies.html',
  styleUrl: './recipies.css'
})
export class Recipies implements OnInit{
  recipes = signal<RecipeModel[]>([]);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string>('');

  constructor(private recipeService: RecipeService) {}

  ngOnInit(): void {
    this.loadAllRecipes();
  }

  loadAllRecipes(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.recipeService.getAllRecipes().subscribe({
      next: (data) => {
        this.recipes.set(data.recipes);
        this.isLoading.set(false);
        console.log('Loaded recipes:', data.recipes);
      },
      error: (err) => {
        console.error('Error loading recipes:', err);
        this.errorMessage.set('Failed to load recipes. Please try again.');
        this.isLoading.set(false);
        this.recipes.set([]);
      },
      complete: () => {
        console.log('Recipe loading completed');
      }
    });
  }
}
