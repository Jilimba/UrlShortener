
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateLinkRequest {
  originalUrl: string;
}

export interface LinkResponse {
  id: string;
  originalUrl: string;
  code: string;
  shortUrl: string;
  createdAtUtc: string;
}

@Injectable({
  providedIn: 'root',
})
export class Link {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5232/api/links';

  createLink(request: CreateLinkRequest): Observable<LinkResponse> {
    return this.http.post<LinkResponse>(this.apiUrl, request);
  }
}
