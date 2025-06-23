export class UserModel {
    constructor(
        public id:number = 0,
        public username:string = '',
        public email:string = '',
        public role:string = '',
        public password:string = '',

    ) {}

    static fromForm(data:{id:number,
        username:string,
        email:string,
        role:string,
        password:string,
        confirm_password:string}) {
            if (data.password === data.confirm_password) {
                return new UserModel(data.id, data.username, data.email, data.role, data.password);
            }
        }
}

