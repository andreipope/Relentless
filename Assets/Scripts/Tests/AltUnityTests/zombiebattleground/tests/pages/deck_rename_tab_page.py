from .base import CZBTests
import time
class Deck_Rename_Tab_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.deck_rename_tab_page=self.get_deck_rename_tab_page()
        self.deck_name_input_field=self.get_deck_name_input_field()
        self.save_button=self.get_save_button()
        
    
    def get_deck_rename_tab_page(self):
        return self.altdriver.wait_for_element('MyDecksPage(Clone)/Tab_Rename')
    def get_deck_name_input_field(self):
        return self.altdriver.wait_for_element(self.deck_rename_tab_page.name+'/Panel_Content/InputText_DeckName')
    def get_save_button(self):
        return self.altdriver.wait_for_element(self.deck_rename_tab_page.name+'/Panel_FrameComponents/Lower_Items/Button_Save')
    def press_save_button(self):
        self.button_pressed(self.save_button)
    

    
     


