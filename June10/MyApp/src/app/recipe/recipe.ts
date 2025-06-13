import { Component, Input } from '@angular/core';
import { RecipeModel } from '../models/recipe';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-recipe',
  imports: [CommonModule],
  templateUrl: './recipe.html',
  styleUrl: './recipe.css'
})
export class Recipe {
  @Input() recipe: RecipeModel | null = null;

  getIngredientsText(): string {
    if (!this.recipe?.ingredients) return '';
    return this.recipe.ingredients.join(', ');
  }
}
