import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DotnetDevelopmentComponent } from './dotnet-development.component';

describe('DotnetDevelopmentComponent', () => {
  let component: DotnetDevelopmentComponent;
  let fixture: ComponentFixture<DotnetDevelopmentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DotnetDevelopmentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DotnetDevelopmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
