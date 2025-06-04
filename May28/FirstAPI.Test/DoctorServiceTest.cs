using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Repositories;
using FirstAPI.Interfaces;
using FirstAPI.Services;
using FirstAPI.Models.DTOs;
using FirstAPI.Models.DTOs;
using FirstAPI.Misc;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using System.Text;

namespace FirstAPI.Test;

public class DoctorServiceTest
{
    private ClinicContext _context;
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ClinicContext>()
                            .UseInMemoryDatabase("TestDb")
                            .Options;
        _context = new ClinicContext(options);
    }
    [TestCase("General")]
    public async Task TestGetDoctorBySpeciality(string speciality)
    {
        Mock<DoctorRepository> doctorRepositoryMock = new Mock<DoctorRepository>(_context);
        Mock<SpecialityRepository> specialityRepositoryMock = new(_context);
        Mock<DoctorSpecialityRepository> doctorSpecialityRepositoryMock = new(_context);
        Mock<UserRepository> userRepositoryMock = new(_context);
        Mock<OtherFunctionalitiesImplementation> otherContextFunctionalitiesMock = new(_context);
        Mock<EncryptionService> encryptionServiceMock = new();
        Mock<IMapper> mapperMock = new();
        Mock<AppointmentRepository> appointmentRepositoryMock = new Mock<AppointmentRepository>(_context);

        otherContextFunctionalitiesMock.Setup(ocf => ocf.GetDoctorsBySpeciality(It.IsAny<string>()))
                                    .ReturnsAsync((string speciality)=>new List<DoctorsBySpecialityResponseDto>{
                                   new DoctorsBySpecialityResponseDto
                                        {
                                            Dname = "test",
                                            Yoe = 2,
                                            Id=1
                                        }
                            });
        IDoctorService doctorService = new DoctorService(doctorRepositoryMock.Object,
                                                        specialityRepositoryMock.Object,
                                                        doctorSpecialityRepositoryMock.Object,
                                                        otherContextFunctionalitiesMock.Object,
                                                        userRepositoryMock.Object,
                                                        encryptionServiceMock.Object,
                                                        mapperMock.Object,
                                                        appointmentRepositoryMock.Object);


        //Assert.That(doctorService, Is.Not.Null);
        //Action
        var result = await doctorService.GetDoctorsBySpeciality(speciality);
        //Assert
        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task AddDoctorPassTest()
    {
        //Arrange
        Mock<DoctorRepository> doctorRepositoryMock = new Mock<DoctorRepository>(_context);
        Mock<SpecialityRepository> specialityRepositoryMock = new(_context);
        Mock<DoctorSpecialityRepository> doctorSpecialityRepositoryMock = new(_context);
        Mock<UserRepository> userRepositoryMock = new(_context);
        Mock<OtherFunctionalitiesImplementation> otherContextFunctionalitiesMock = new(_context);
        Mock<EncryptionService> encryptionServiceMock = new();
        Mock<IMapper> mapperMock = new();
        Mock<AppointmentRepository> appointmentRepositoryMock = new Mock<AppointmentRepository>(_context);
        Mock<DoctorMapper> doctorMapperMock = new Mock<DoctorMapper>(_context);
        Mock<SpecialityMapper> specialityMapperMock = new Mock<SpecialityMapper>(_context);


        mapperMock.Setup(m => m.Map<DoctorAddRequestDto, User>(It.IsAny<DoctorAddRequestDto>()))
                    .Returns(new User
                    {
                        Username = "test@gmail.com",
                        Role = "Doctor"
                    });

        encryptionServiceMock.Setup(es => es.EncryptData(It.IsAny<EncryptModel>()))
                            .ReturnsAsync(new EncryptModel
                            {
                                Data = "testpassword",
                                EncryptedData = Encoding.UTF8.GetBytes("testpassword"),
                                HashKey = Guid.NewGuid().ToByteArray()
                            });

        doctorMapperMock.Setup(dm => dm.MapDoctorAddRequestDoctor(It.IsAny<DoctorAddRequestDto>()))
                        .Returns(new Doctor
                        {
                            Name = "Test",
                            YearsOfExperience = 20,
                            Email = "test@gmail.com"
                        });

        userRepositoryMock.Setup(ur => ur.Add(It.IsAny<User>()))
                            .ReturnsAsync(new User
                            {
                                Username = "test@gmail.com",
                                Password = Encoding.UTF8.GetBytes("testpassword"),
                                HashKey = Guid.NewGuid().ToByteArray(),
                                Role = "Doctor"
                            });


        specialityMapperMock.Setup(sm => sm.MapDoctorSpeciality(It.IsAny<int>(), It.IsAny<int>()))
                            .Returns(new DoctorSpeciality
                            {
                                DoctorId = 1,
                                SpecialityId = 1,
                            });

        doctorSpecialityRepositoryMock.Setup(dsr => dsr.Add(It.IsAny<DoctorSpeciality>()))
                                        .ReturnsAsync(new DoctorSpeciality
                                        {
                                            DoctorId = 1,
                                            SpecialityId = 1
                                        });

        specialityRepositoryMock.Setup(sr => sr.GetAll())
                                .ReturnsAsync(new List<Speciality>
                                {
                                    new Speciality {
                                        Id =1,
                                        Name = "General",
                                    }
                                });
        specialityMapperMock.Setup(sm => sm.MapSpecialityAddRequestDoctor(It.IsAny<SpecialityAddRequestDto>()))
                            .Returns(new Speciality
                            {
                                Id = 1,
                                Name = "General"
                            });

        specialityRepositoryMock.Setup(sr => sr.Add(It.IsAny<Speciality>()))
                                .ReturnsAsync(new Speciality
                                {
                                    Id = 1,
                                    Name = "General"
                                });

        IDoctorService doctorService = new DoctorService(doctorRepositoryMock.Object,
                                                        specialityRepositoryMock.Object,
                                                        doctorSpecialityRepositoryMock.Object,
                                                        otherContextFunctionalitiesMock.Object,
                                                        userRepositoryMock.Object,
                                                        encryptionServiceMock.Object,
                                                        mapperMock.Object,
                                                        appointmentRepositoryMock.Object);

        //Action
        var result = await doctorService.AddDoctor(new DoctorAddRequestDto
        {
            Name = "Test",
            Specialities = new List<SpecialityAddRequestDto>
            {
                new SpecialityAddRequestDto {Name="General"}
            },
            YearsOfExperience = 20,
            Email = "test@gmail.com",
            Password = "testpassword"
        });

        //Assert 
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task MapAndAddSpecialityPassTest()
    {
        //Arrange
        Mock<DoctorRepository> doctorRepositoryMock = new Mock<DoctorRepository>(_context);
        Mock<SpecialityRepository> specialityRepositoryMock = new(_context);
        Mock<DoctorSpecialityRepository> doctorSpecialityRepositoryMock = new(_context);
        Mock<UserRepository> userRepositoryMock = new(_context);
        Mock<OtherFunctionalitiesImplementation> otherContextFunctionalitiesMock = new(_context);
        Mock<EncryptionService> encryptionServiceMock = new();
        Mock<IMapper> mapperMock = new();
        Mock<AppointmentRepository> appointmentRepositoryMock = new Mock<AppointmentRepository>(_context);
        Mock<DoctorMapper> doctorMapperMock = new Mock<DoctorMapper>(_context);
        Mock<SpecialityMapper> specialityMapperMock = new Mock<SpecialityMapper>(_context);

        specialityRepositoryMock.Setup(sr => sr.GetAll())
                                .ReturnsAsync(new List<Speciality>
                                {
                                    new Speciality {
                                        Id =1,
                                        Name = "General",
                                    }
                                });
        specialityMapperMock.Setup(sm => sm.MapSpecialityAddRequestDoctor(It.IsAny<SpecialityAddRequestDto>()))
                            .Returns(new Speciality
                            {
                                Id = 1,
                                Name = "General"
                            });

        specialityRepositoryMock.Setup(sr => sr.Add(It.IsAny<Speciality>()))
                                .ReturnsAsync(new Speciality
                                {
                                    Id = 1,
                                    Name = "General"
                                });

        IDoctorService doctorService = new DoctorService(doctorRepositoryMock.Object,
                                                        specialityRepositoryMock.Object,
                                                        doctorSpecialityRepositoryMock.Object,
                                                        otherContextFunctionalitiesMock.Object,
                                                        userRepositoryMock.Object,
                                                        encryptionServiceMock.Object,
                                                        mapperMock.Object,
                                                        appointmentRepositoryMock.Object);

        //Action 
        var result = await doctorService.MapAndAddSpeciality(new DoctorAddRequestDto
        {
            Name = "Test",
            Specialities = new List<SpecialityAddRequestDto>
            {
                new SpecialityAddRequestDto {Name="General"}
            },
            YearsOfExperience = 20,
            Email = "test@gmail.com",
            Password = "testpassword"
        });

        //Assert
        Assert.That(result, Is.Not.Null);

    }

    [Test]
    public async Task ViewDoctorAppointmentsPassTest()
    {
        //Arrange
        Mock<DoctorRepository> doctorRepositoryMock = new Mock<DoctorRepository>(_context);
        Mock<SpecialityRepository> specialityRepositoryMock = new(_context);
        Mock<DoctorSpecialityRepository> doctorSpecialityRepositoryMock = new(_context);
        Mock<UserRepository> userRepositoryMock = new(_context);
        Mock<OtherFunctionalitiesImplementation> otherContextFunctionalitiesMock = new(_context);
        Mock<EncryptionService> encryptionServiceMock = new();
        Mock<IMapper> mapperMock = new();
        Mock<AppointmentRepository> appointmentRepositoryMock = new Mock<AppointmentRepository>(_context);
        Mock<DoctorMapper> doctorMapperMock = new Mock<DoctorMapper>(_context);
        Mock<SpecialityMapper> specialityMapperMock = new Mock<SpecialityMapper>(_context);

        doctorRepositoryMock.Setup(dr => dr.GetByID(It.IsAny<int>()))
                            .ReturnsAsync(new Doctor
                            {
                                Id = 1,
                                Name = "test",
                                YearsOfExperience = 20,
                                Email = "test@gmail.com",
                                Appointments = new List<Appointment>
                                { new Appointment{
                                    Id = 1,
                                    PatientId = 1,
                                    DoctorId = 1,
                                    AppointmentDate = DateTime.Now,
                                    Status = "Upcoming"
                                    }
                                }

                            });

        IDoctorService doctorService = new DoctorService(doctorRepositoryMock.Object,
                                                        specialityRepositoryMock.Object,
                                                        doctorSpecialityRepositoryMock.Object,
                                                        otherContextFunctionalitiesMock.Object,
                                                        userRepositoryMock.Object,
                                                        encryptionServiceMock.Object,
                                                        mapperMock.Object,
                                                        appointmentRepositoryMock.Object);

        //Action
        var result = doctorService.ViewAppointments(1);

        //Assert 
        Assert.That(result, Is.Not.Null);

        
    }


    [TearDown]
    public void TearDown() 
    {
        _context.Dispose();
    }


}