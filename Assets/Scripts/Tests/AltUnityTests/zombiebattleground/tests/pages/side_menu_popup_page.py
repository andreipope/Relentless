from .base import CZBTests

class Side_Menu_Popup_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.side_menu_popup_page=self.get_side_menu_popup_page()
        self.battle_button=self.get_battle_button()
        self.shop_button=self.get_shop_button()
        self.open_packs_button=self.get_open_packs_button()
        self.my_cards_button=self.get_my_cards_button()
        self.my_deck_button=self.get_my_decks_button()
    
    def get_side_menu_popup_page(self):
        return self.altdriver.wait_for_element('SideMenuPopup(Clone)')
    def get_my_decks_button(self):
        return self.altdriver.wait_for_element(self.side_menu_popup_page.name+'/Group/Button_MyDecks')
    def get_battle_button(self):
        return self.altdriver.wait_for_element(self.side_menu_popup_page.name+'/Group/Button_Battle')
    def get_shop_button(self):
        return self.altdriver.wait_for_element(self.side_menu_popup_page.name+'/Group/Button_Shop')
    def get_open_packs_button(self):
        return self.altdriver.wait_for_element(self.side_menu_popup_page.name+'/Group/Button_MyPacks')
    def get_my_cards_button(self):
        return self.altdriver.wait_for_element(self.side_menu_popup_page.name+'/Group/Button_MyCards')

    def press_shop_button(self):
        self.button_pressed(self.shop_button)
    def press_my_decks_button(self):
        self.button_pressed(self.my_deck_button)
    def press_open_packs_button(self):
        self.button_pressed(self.open_packs_button)
    def press_battle_button(self):
        self.button_pressed(self.battle_button)
    def press_my_cards_button(self):
        self.button_pressed(self.open_packs_button)
