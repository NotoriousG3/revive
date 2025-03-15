class Cart {
    items = {}

    constructor() {
        this.TotalElement = $('#cartTotal');
        this.UpdateTotal();
    }

    static ToLocaleString(value) {
        return value.toLocaleString('en-US');
    }

    AddItem(module) {
        this.items[module.Id] = new CartItem(module.Id, module.Name, module.SnapWebIconClass, 1, module.Price);
        this.UpdateTotal();
    }

    RemoveItem(moduleId) {
        let cartItem = this.items[moduleId];
        $(cartItem.Element).remove();
        delete this.items[moduleId];
        this.UpdateTotal();
    }

    AddQuantity(moduleId, quantity) {
        this.items[moduleId].AddQuantity(quantity);
        this.UpdateTotal();
    }

    SetQuantity(moduleId, quantity) {
        this.items[moduleId].SetQuantity(quantity);
        this.UpdateTotal();
    }

    ShowInvoiceTable(containerSelector = "#cartItemsContainer") {
        let cartContainer = $(containerSelector);
        cartContainer.empty();
        for (const [moduleId, cartItem] of Object.entries(this.items)) {
            let newElement = cartItem.CreateElement();
            cartContainer.append(newElement);
        }
    }

    CalculateTotal() {
        return Object.values(this.items).reduce((accumulator, ci) => accumulator + ci.Total(), 0);
    }

    UpdateTotal() {
        this.TotalElement.text(Cart.ToLocaleString(this.CalculateTotal()));
    }
}

let cart = new Cart();

class CartItem {
    constructor(moduleId, name, iconClass, quantity, price) {
        this.ModuleId = moduleId;
        this.Name = name;
        this.Quantity = quantity;
        this.Price = price;
        this.IconClass = iconClass;
    }

    Total() {
        return this.Quantity * this.Price;
    }

    CreateElement() {
        this.Element = $('#cartItem').clone()
        this.Element.removeAttr('id');
        this.Element.toggleClass('d-none');
        this.Element.find('.cartItemModuleId').val(this.ModuleId);
        let iconContainer = this.Element.find('.cartItemIcon');
        iconContainer.empty();
        iconContainer.append($(`<i class="${this.IconClass}"></i>`));
        this.UpdateContents();

        return this.Element;
    }

    UpdateContents() {
        this.Element.find('.cartItemName').text(this.Name);
        this.Element.find('.cartItemPrice').text(`$${this.Price}`);
        this.Element.find('.cartItemTotalPrice').text(`$${Cart.ToLocaleString(this.Total())}`);
        this.Element.find('.cartItemQuantity').val(this.Quantity);
    }

    AddQuantity(change) {
        this.Quantity += change;

        if (this.Quantity < 0) {
            this.Quantity = 0;
            return;
        }
        this.UpdateContents();
    }

    SetQuantity(value) {
        value = value < 0 ? 0 : value;
        this.Quantity = parseInt(value);
        this.UpdateContents();
    }
}

function AddToCart(buttonElement, module, unique = false) {
    if (cart.items[module.Id] == undefined) {
        cart.AddItem(module);
        cart.ShowInvoiceTable();
        return;
    }

    // We only allow one of these
    if (unique) return;

    cart.AddQuantity(module.Id, 1)
    cart.ShowInvoiceTable();
}

function RemoveItem(buttonElement) {
    let moduleId = $(buttonElement).closest('.cartItem').find('.cartItemModuleId').val();
    cart.RemoveItem(moduleId);
}

function AddItem(button) {
    let moduleId = $(button).closest('.cartItem').find('.cartItemModuleId').val();
    cart.AddQuantity(moduleId, 1);
}

function MinusItem(button) {
    let moduleId = $(button).closest('.cartItem').find('.cartItemModuleId').val();
    cart.AddQuantity(moduleId, -1);
}

function UpdateCartItemQuantity(input) {
    let moduleId = $(input).closest('.cartItem').find('.cartItemModuleId').val();
    cart.SetQuantity(moduleId, $(input).val());
}

function CreateInvoice() {
    let data = [];

    for (const [moduleId, cartItem] of Object.entries(cart.items)) {
        let purchaseInfo = {ModuleId: moduleId, Quantity: cartItem.Quantity};
        data.push(purchaseInfo);
    }

    $.post("/api/purchase", {PurchaseInfo: data}, (response) => {
        if (response.code != 0) console.log("error");

        window.btcpay.showInvoice(response.data.id);
    });
}

function ShowInvoiceCheckout(invoiceId) {
    window.btcpay.showInvoice(invoiceId);
}