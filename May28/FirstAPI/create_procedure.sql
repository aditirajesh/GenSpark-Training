create or replace function proc_GetDoctorsBySpeciality(spciality varchar(20))
returns table(id int,dname text,yoe real)
as 
$$
Begin
   return query select distinct "Id","Name","YearsOfExperience" from public."Doctors"
   where "Id" in(SELECT "DoctorId" FROM public."DoctorSpecialities" where "SpecialityId" in
			  (select "Id" from public."Specialities" where "Name"=spciality));
End;
$$
Language plpgsql;

drop function proc_GetDoctorsBySpeciality(spciality varchar(20))

SELECT * FROM public."Doctors"
select * from public."Specialities" where "Name"='Cardiology'

select * from proc_GetDoctorsBySpeciality('Cardiology')

select distinct("Id","Name","YearsOfExperience") from public."Doctors"
   where "Id" in(SELECT "DoctorId" FROM public."DoctorSpecialities" where "SpecialityId" in
			  (select "Id" from public."Specialities" where "Name"='Cardiology'));