import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";
import type { Observable } from "rxjs";

export interface ConsultorioMEProfessional {
  proId: string;
  id: string;
  name: string;
  extraInfo: string;
  info: string;
  speciality: string;
  groupSpeciality: string;
}

@Injectable({ providedIn: "root" })
export class ConsultorioMEService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/consultoriome`;

  getProfessionals(): Observable<ConsultorioMEProfessional[]> {
    return this.http.get<ConsultorioMEProfessional[]>(`${this.baseUrl}/professionals`);
  }
}
