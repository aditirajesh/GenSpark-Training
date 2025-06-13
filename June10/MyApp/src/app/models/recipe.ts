export class RecipeModel {
    constructor(
        public id:number,
        public image:string,
        public name:string,
        public cuisine:string,
        public cookTimeMinutes:number,
        public ingredients:string[]
    ) {}
}