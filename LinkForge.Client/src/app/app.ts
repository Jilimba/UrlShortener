
import { Component, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Link, LinkResponse } from './services/link';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  originalUrl = '';
  result: LinkResponse | null = null;
  loading = false;
  errorMessage = '';
  copied = false;

  constructor(
    private linkService: Link,
    private cdr: ChangeDetectorRef
  ) {}

  shortenUrl(): void {
    const url = this.originalUrl.trim();

    this.result = null;
    this.errorMessage = '';
    this.copied = false;

    if (!url) {
      this.errorMessage = 'Please enter a URL first.';
      return;
    }

    try {
      const parsedUrl = new URL(url);

      if (
        parsedUrl.protocol !== 'http:' &&
        parsedUrl.protocol !== 'https:'
      ) {
        this.errorMessage =
          'Please enter a valid HTTP or HTTPS URL.';
        return;
      }
    } catch {
      this.errorMessage =
        'Please enter a valid URL, including https://.';
      return;
    }

    this.loading = true;

    this.linkService
      .createLink({ originalUrl: url })
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (response) => {
          console.log('API response:', response);
          this.result = response;
          this.cdr.markForCheck();
        },
        error: (error) => {
          console.error('Failed to shorten URL:', error);

          this.errorMessage =
            'We could not shorten your link. Please check that the API is running and try again.';

          this.cdr.markForCheck();
        }
      });
  }

  async copyLink(): Promise<void> {
    if (!this.result) {
      return;
    }

    try {
      await navigator.clipboard.writeText(
        this.result.shortUrl
      );

      this.copied = true;
      this.errorMessage = '';
      this.cdr.markForCheck();
    } catch {
      this.errorMessage =
        'Copy failed. Please select and copy the link manually.';
      this.cdr.markForCheck();
    }
  }
}
