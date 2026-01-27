import { Injectable, signal } from '@angular/core'
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class MaintenanceService {
    private maintenanceSignal = signal<boolean>(environment.maintenanceMode);

    get isInMaintenanceMode() {
        return this.maintenanceSignal.asReadonly();
    }

    enableMaintenance() {
        this.maintenanceSignal.set(true);
    }

    disableMaintenanceMode() {
        this.maintenanceSignal.set(false);
    }
}