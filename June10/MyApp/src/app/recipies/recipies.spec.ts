import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Recipies } from './recipies';

describe('Recipies', () => {
  let component: Recipies;
  let fixture: ComponentFixture<Recipies>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Recipies]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Recipies);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
