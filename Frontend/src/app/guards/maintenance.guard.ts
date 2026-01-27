import { inject } from "@angular/core";
import { Router, type CanActivateFn } from '@angular/router'
import { MaintenanceService} from '../services/maintenance.service'

export const MaintenanceGuard : CanActivateFn = (route, state) => {
    const maintenanceService = inject(MaintenanceService);
    const router = inject(Router);

    if (state.url === '/maintenance') {
        return true;
    }

    if (maintenanceService.isInMaintenanceMode()) {
        return router.parseUrl('/maintenance');
    }

    return true;
}