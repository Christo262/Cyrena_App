import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Package } from '../models/extensionmodel.model';

@Injectable({
  providedIn: 'root'
})
export class ExtensionDataService {
  private readonly http = inject(HttpClient);

  getPackage(applicationId: string, packageId: string): Observable<Package> {
    return this.http.get<Package>(`/api/applications/${applicationId}/packages/${packageId}`);
  }

  getPackages(applicationId: string, os?: string, arch?: string): Observable<Package[]> {
    let url = `/api/applications/${applicationId}/packages`;
    const params = new URLSearchParams();
    if (os) params.set('os', os);
    if (arch) params.set('arch', arch);
    const query = params.toString();
    if (query) url += `?${query}`;
    return this.http.get<Package[]>(url);
  }

  getIconUrl(applicationId: string, packageId: string): string {
    return `/api/applications/${applicationId}/packages/${packageId}/icon`;
  }

  getDownloadUrl(applicationId: string, packageId: string, version?: string): string {
    let url = `/api/applications/${applicationId}/packages/${packageId}/download`;
    if (version) url += `?version=${encodeURIComponent(version)}`;
    return url;
  }
}
