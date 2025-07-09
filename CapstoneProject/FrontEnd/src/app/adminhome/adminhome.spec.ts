import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Adminhome } from './adminhome';

describe('Adminhome', () => {
  let component: Adminhome;
  let fixture: ComponentFixture<Adminhome>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Adminhome]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Adminhome);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
