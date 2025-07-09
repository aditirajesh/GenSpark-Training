import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Userprofile } from './userprofile';

describe('Userprofile', () => {
  let component: Userprofile;
  let fixture: ComponentFixture<Userprofile>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Userprofile]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Userprofile);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
