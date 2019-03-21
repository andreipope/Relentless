from .base import CZBTests

class Tutorial_Popup_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.tutorial_popup_page=self.get_side_menu_popup_page()
        self.battle_button=self.get_battle_button()
        self.shop_button=self.get_shop_button()
        self.open_packs_button=self.get_open_packs_button()
        self.my_cards_button=self.get_my_cards_button()
        self.my_deck_button=self.get_my_decks_button()
    
    def get_side_menu_popup_page(self):
        return self.altdriver.wait_for_element('SideMenuPopup(Clone)')
    def get_my_decks_button(self):
        return self.altdriver.wait_for_element(self.tutorial_popup_page.name+'/Group/Button_MyDecks')
    def get_battle_button(self):
        return self.altdriver.wait_for_element(self.tutorial_popup_page.name+'/Group/Button_Battle')
   

    def press_No_button(self):
        self.button_pressed(self.shop_button)
    def press_Yes_button(self):
        self.button_pressed(self.my_deck_button)
    
