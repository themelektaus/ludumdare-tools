import Const from "./const"

export default class Storage
{
    static get token()
    {
        return localStorage.getItem("token") || ""
    }
    
    static set token(value)
    {
        localStorage.setItem("token", value)
    }
    
    static removeToken()
    {
        localStorage.removeItem("token")
    }
    
    static get ld()
    {
        return +(localStorage.getItem("ld") || Const.MAX_LD_NUMBER)
    }
    
    static set ld(value)
    {
        localStorage.setItem("ld", value)
    }
    
    static get search()
    {
        return localStorage.getItem("search") || ""
    }
    
    static set search(value)
    {
        localStorage.setItem("search", value)
    }
    
    static get options()
    {
        const json = localStorage.getItem("options") || JSON.stringify({
            filterJam: true,
            filterCompo: true,
            filterRated: true,
            filterUnrated: true,
            orderCategory: "smart"
        })
        return JSON.parse(json)
    }
    
    static set options(value)
    {
        localStorage.setItem("options", JSON.stringify(value))
    }
}