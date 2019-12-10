import {Photo} from './photo';
import { from } from 'rxjs';
export interface User {
    int: number;
    username: string;
    knownAs: string;
    age: number;
    gender: string;
    created: Date;
    lastActive: Date;
    photoUrl: string;
    city: string;
    country: string;
    interesrs?: string;
    introduction?: string;
    lookingFor?: string;
    photos?: Photo[];
}
