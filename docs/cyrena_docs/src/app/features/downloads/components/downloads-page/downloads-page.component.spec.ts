import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DownloadsPageComponent } from './downloads-page.component';

describe('DownloadsPageComponent', () => {
  let component: DownloadsPageComponent;
  let fixture: ComponentFixture<DownloadsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DownloadsPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DownloadsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
