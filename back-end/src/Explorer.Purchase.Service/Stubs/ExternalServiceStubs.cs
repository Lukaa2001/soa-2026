using Explorer.Payments.API.Internal;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Internal;
using FluentResults;

namespace Explorer.Purchase.Service.Stubs;

// Out-of-scope coupon logic is stubbed. The Tour lookup used at checkout
// (Purchase -> Tours) gets a real gRPC client in Faza 7.

public class TourServiceStub : IInternalTourService
{
    public Result<TourDto> GetById(long id) => Result.Ok(new TourDto());
}

public class CouponServiceStub : IInternalCouponService
{
    public bool IsCoupnValid(string couponHash) => false;
    public bool CheckIfAutorApplies(long id, string couponHash) => false;
    public void SetCouponToInvalid(string couponHash) { }
    public bool CheckIfIsApplicableToAll(string couponHash) => false;
    public double GetDiscounyByCouponHash(string couponHash) => 0;
    public int GetTourByCouponHash(string couponHash) => 0;
}
