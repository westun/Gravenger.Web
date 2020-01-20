using System.ComponentModel.DataAnnotations;

namespace Gravenger.Areas.Admin.ViewModels
{
    public class InvitationListVM
    {
        public InvitationVM[] Invitations { get; set; }
    }
}