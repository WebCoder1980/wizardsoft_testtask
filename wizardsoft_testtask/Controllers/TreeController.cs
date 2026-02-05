using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wizardsoft_testtask.Dtos;
using wizardsoft_testtask.Service;

namespace wizardsoft_testtask.Controllers
{
    [ApiController]
    public class TreeController : ControllerBase
    {
        private readonly ITreeService _service;

        public TreeController(ITreeService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<TreeNodeResponse>> GetById(long id, CancellationToken cancellationToken)
        {
            var node = await _service.GetAsync(id, cancellationToken);
            if (node == null)
            {
                return NotFound();
            }

            return Ok(node);
        }

        [HttpGet("export")]
        public async Task<ActionResult<IEnumerable<TreeNodeResponse>>> Export(CancellationToken cancellationToken)
        {
            var tree = await _service.ExportAsync(cancellationToken);
            return Ok(tree);
        }
    }
}
