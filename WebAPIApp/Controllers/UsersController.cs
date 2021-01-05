using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace WebAPIApp.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserStore _db;

        public UsersController(IUserStore db)
        {
            _db = db;

            if (db.Get().Any()) return;

            db.Save(new UserModel
            {
                Name = "Bill",
                Age = 35
            });

            db.Save(new UserModel
            {
                Name = "Bob",
                Age = 37
            });
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserModel>> Get()
        {
            return _db.Get();
        }

        [HttpGet("{id}")]
        public ActionResult<UserModel> Get(int id)
        {
            UserModel res = _db.Get().FirstOrDefault(x => x.Id == id);

            if (res == null)
            {
                return NotFound(new { id });
            }

            return new ObjectResult(res);
        }

        [HttpPost]
        public ActionResult<int> Post(UserModel model)
        {
            if (model == null)
            {
                return BadRequest($"{nameof(UserModel)} is null");
            }

            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            int id = _db.Save(model);

            return Ok(new { id });
        }

        [HttpPut]
        public ActionResult<int> Put(UserModel model)
        {
            if (model == null)
            {
                return BadRequest($"{nameof(UserModel)} is null");
            }
            
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            if (model.Id == 0)
            {
                return BadRequest($"{nameof(UserModel)}.{nameof(UserModel.Id)} must be not zero");
            }

            if (_db.Get().All(x => x.Id != model.Id))
            {
                return NotFound(new { id = model.Id });
            }

            _db.Save(model);

            return Ok(new { id = model.Id });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return BadRequest($"{nameof(id)} must be not zero");
            }

            if (_db.Get().All(x => x.Id != id))
            {
                return NotFound(new { id });
            }

            _db.Delete(id);

            return Ok();
        }
    }
}
