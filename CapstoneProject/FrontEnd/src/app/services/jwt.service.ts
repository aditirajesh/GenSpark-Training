import { Injectable } from "@angular/core";

@Injectable({
    providedIn:'root'
})
export class JwtService{

    //decodetoken
    decodeToken(token:string):any {
        try {
            const payload = token.split('.')[1];
            const decoded = JSON.parse(atob(payload));
            return decoded;
        } catch(error) {
            console.error('Error decoding JWT token:', error);
            return null;
        }
    }

    //isExpired
    isTokenExpired(token:string):boolean {
        const decoded = this.decodeToken(token);
        if (!decoded || !decoded.exp) {
            return true;
        }
        const expiration_date = new Date(decoded.exp * 1000);
        return expiration_date <= new Date();
    }

    //getExpiration 
    getExpiration(token:string):Date|null {
        const decoded = this.decodeToken(token);
        if (!decoded || !decoded.exp) {
            return null;
        }
        return new Date(decoded.exp*1000);
    }

    //extractUser - Fixed for .NET backend
    extractUserFromToken(token: string): any {
        const decoded = this.decodeToken(token);
        if (!decoded) {
            return null;
        }

        // Debug: Log the full JWT payload to understand the structure
        console.log('=== JWT PAYLOAD DEBUG ===');
        console.log('Full JWT payload:', decoded);
        console.log('Available claim keys:', Object.keys(decoded));
        console.log('========================');

        // .NET Core JWT tokens might use different claim names
        // Let's check all possible variations
        const possibleUsernameClaims = [
            // Standard Microsoft claim URIs
            'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
            'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
            
            // Short form claims (sometimes used)
            'name',
            'unique_name',
            'nameid',
            'sub',
            'email',
            'username',
            
            // Custom claims your backend might be using
            'user_name',
            'userName'
        ];

        const possibleRoleClaims = [
            // Standard Microsoft role claim URI
            'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
            
            // Short form claims
            'role',
            'roles'
        ];

        // Find username from available claims
        let username = null;
        for (const claim of possibleUsernameClaims) {
            if (decoded[claim]) {
                username = decoded[claim];
                console.log(`Found username in claim '${claim}': ${username}`);
                break;
            }
        }

        // Find role from available claims
        let role = null;
        for (const claim of possibleRoleClaims) {
            if (decoded[claim]) {
                role = decoded[claim];
                console.log(`Found role in claim '${claim}': ${role}`);
                break;
            }
        }

        // Log what we found
        console.log('Extracted username:', username);
        console.log('Extracted role:', role);

        return {
            username: username,
            role: role,
            exp: decoded.exp,
            iat: decoded.iat
        };
    }
}