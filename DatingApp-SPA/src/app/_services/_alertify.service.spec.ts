/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { _alertifyService } from './alertify.service';

describe('Service: _alertify', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [_alertifyService]
    });
  });

  it('should ...', inject([_alertifyService], (service: _alertifyService) => {
    expect(service).toBeTruthy();
  }));
});
