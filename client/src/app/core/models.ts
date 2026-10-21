export interface TaskItem {
	id: string;
	title: string;
	description?: string;
	isCompleted: boolean;
	owner: string;
	createdAt: string;
}

export interface Skin {
	id: string;
	name: string;
	primaryColor: string;
	accentColor: string;
	backgroundColor: string;
	fontFamily: string;
	isActive: boolean;
}

export interface LoginRequest {
	username: string;
	password: string;
}

export interface LoginResponse {
	token: string;
	username: string;
	role: string;
	permissions: string[];
	expiresAt: string;
}
