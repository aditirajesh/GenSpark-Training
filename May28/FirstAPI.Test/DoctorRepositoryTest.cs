using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Repositories;
using FirstAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;



namespace FirstAPI.Test;

public class DoctorRepositoryTest
{
    private ClinicContext _context;
    private IRepository<int, Doctor> _doctorRepository;
    private IRepository<string, User> _userRepository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ClinicContext>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;

        _context = new ClinicContext(options);
        _doctorRepository = new DoctorRepository(_context);
        _userRepository = new UserRepository(_context);
    }

    [Test]
    public async Task AddDoctorPassTest()
    {
        //Arrange 
        var email = "test@gmail.com";
        var password = Encoding.UTF8.GetBytes("testpassword");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User()
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Doctor"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        var doctor = new Doctor()
        {
            Name = "test",
            YearsOfExperience = 20,
            Email = email,
        };

        //Action 
        var result = await _doctorRepository.Add(doctor);

        //Assert 
        Assert.That(result, Is.Not.Null);

    }

    [Test]
    public async Task UpdateDoctorPassTest()
    {
        //Arrange
        var email = "test@gmail.com";
        var password = Encoding.UTF8.GetBytes("testpassword");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User()
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Doctor"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        var doctor = new Doctor()
        {
            Name = "test",
            YearsOfExperience = 20,
            Email = email,
        };
        var updatedoctor = await _doctorRepository.Add(doctor);
        updatedoctor.YearsOfExperience = 30;

        //Action 
        var result = await _doctorRepository.Update(updatedoctor.Id, updatedoctor);

        //Assert
        Assert.That(result.YearsOfExperience, Is.EqualTo(30));

    }

    [Test]
    public async Task DeleteDoctorPassTest()
    {
        //Arrange
        var email = "test@gmail.com";
        var password = Encoding.UTF8.GetBytes("testpassword");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User()
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Doctor"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        var doctor = new Doctor()
        {
            Name = "test",
            YearsOfExperience = 20,
            Email = email,
        };
        var addeddoctor = await _doctorRepository.Add(doctor);

        //Action
        var result = await _doctorRepository.Delete(addeddoctor.Id);

        //Assert 
        Assert.That(async () => await _doctorRepository.GetByID(result.Id),
            Throws.TypeOf<Exception>());
        
    }

    [Test]
    public async Task GetDoctorPassTest()
    {
        //Arrange
        var email = "test@gmail.com";
        var password = Encoding.UTF8.GetBytes("testpassword");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User()
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Doctor"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        var doctor = new Doctor()
        {
            Name = "test",
            YearsOfExperience = 20,
            Email = email,
        };
        var addeddoctor = await _doctorRepository.Add(doctor);

        //Action 
        var result = await _doctorRepository.GetByID(addeddoctor.Id);

        //Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetAllDoctorsPassTest()
    {
        var email = "test@gmail.com";
        var password = Encoding.UTF8.GetBytes("testpassword");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User()
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Doctor"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        var doctor = new Doctor()
        {
            Name = "test",
            YearsOfExperience = 20,
            Email = email,
        };

        var email2 = "test2@gmail.com";
        var password2 = Encoding.UTF8.GetBytes("test2password");
        var key2 = Guid.NewGuid().ToByteArray();
        var user2 = new User()
        {
            Username = email2,
            Password = password2,
            HashKey = key2,
            Role = "Doctor"
        };
        _context.Add(user2);
        await _context.SaveChangesAsync();

        var doctor2 = new Doctor()
        {
            Name = "test2",
            YearsOfExperience = 20,
            Email = email2,
        };
        await _doctorRepository.Add(doctor);
        await _doctorRepository.Add(doctor2);

        //Action
        var result = await _doctorRepository.GetAll();

        //Assert
        Assert.That(result,Is.Not.Null);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
}